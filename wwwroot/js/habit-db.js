window.habitDb = (() => {
  let closePromptRegistered = false;

  const openDb = (dbName, version, stores, ensureStores) => new Promise((resolve, reject) => {
    const request = typeof version === "number" ? indexedDB.open(dbName, version) : indexedDB.open(dbName);

    request.onupgradeneeded = (event) => {
      if (!ensureStores) {
        return;
      }

      const db = event.target.result;
      stores.forEach((store) => {
        if (db.objectStoreNames.contains(store)) {
          const existing = event.target.transaction.objectStore(store);
          if (existing.keyPath !== "id") {
            db.deleteObjectStore(store);
            db.createObjectStore(store, { keyPath: "id" });
          }
          return;
        }

        db.createObjectStore(store, { keyPath: "id" });
      });
    };

    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });

  const deleteDb = (dbName) => new Promise((resolve, reject) => {
    const request = indexedDB.deleteDatabase(dbName);
    request.onsuccess = () => resolve();
    request.onerror = () => reject(request.error);
    request.onblocked = () => reject(new Error("Database delete blocked by another tab."));
  });

  const txStore = (db, storeName, mode) => db.transaction(storeName, mode).objectStore(storeName);

  const reqToPromise = (req) => new Promise((resolve, reject) => {
    req.onsuccess = () => resolve(req.result);
    req.onerror = () => reject(req.error);
  });

  return {
    ensureDb: async (dbName, version, stores) => {
      let db = await openDb(dbName, version, stores, true);

      let needsReset = false;
      for (const store of stores) {
        if (!db.objectStoreNames.contains(store)) {
          needsReset = true;
          break;
        }

        const keyPath = db.transaction(store, "readonly").objectStore(store).keyPath;
        if (keyPath !== "id") {
          needsReset = true;
          break;
        }
      }

      if (needsReset) {
        db.close();
        await deleteDb(dbName);
        db = await openDb(dbName, version, stores, true);
      }

      db.close();
    },

    getAll: async (dbName, storeName) => {
      const db = await openDb(dbName, undefined, [storeName], false);
      const req = txStore(db, storeName, "readonly").getAll();
      const rows = await reqToPromise(req);
      db.close();
      return rows ?? [];
    },

    upsert: async (dbName, storeName, entity) => {
      const db = await openDb(dbName, undefined, [storeName], false);
      const req = txStore(db, storeName, "readwrite").put(entity);
      await reqToPromise(req);
      db.close();
    },

    deleteRecord: async (dbName, storeName, id) => {
      const db = await openDb(dbName, undefined, [storeName], false);
      const req = txStore(db, storeName, "readwrite").delete(id);
      await reqToPromise(req);
      db.close();
    },

    clearStore: async (dbName, storeName) => {
      const db = await openDb(dbName, undefined, [storeName], false);
      const req = txStore(db, storeName, "readwrite").clear();
      await reqToPromise(req);
      db.close();
    },

    exportAll: async (dbName, stores) => {
      const db = await openDb(dbName, undefined, stores, false);
      const output = {};
      for (const store of stores) {
        const req = txStore(db, store, "readonly").getAll();
        output[store] = await reqToPromise(req);
      }
      db.close();
      return output;
    },

    importAll: async (dbName, data, stores) => {
      const db = await openDb(dbName, undefined, stores, false);
      for (const store of stores) {
        const objectStore = txStore(db, store, "readwrite");
        await reqToPromise(objectStore.clear());
        const rows = data[store] || [];
        for (const row of rows) {
          await reqToPromise(objectStore.put(row));
        }
      }
      db.close();
    },

    applyTheme: (mode) => {
      const actual = mode || "light";
      document.documentElement.setAttribute("data-theme", actual);
      localStorage.setItem("habit-theme", actual);
    },

    getTheme: () => localStorage.getItem("habit-theme") || "light",

    downloadJson: (filename, text) => {
      const blob = new Blob([text], { type: "application/json" });
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = filename;
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      URL.revokeObjectURL(url);
    },

    enableBeforeCloseExportPrompt: () => {
      if (closePromptRegistered) {
        return;
      }

      closePromptRegistered = true;
      window.addEventListener("beforeunload", (event) => {
        // Browser usually ignores custom text and shows a generic confirmation dialog.
        event.preventDefault();
        event.returnValue = "Export your data from Settings before closing.";
        return event.returnValue;
      });
    }
  };
})();
