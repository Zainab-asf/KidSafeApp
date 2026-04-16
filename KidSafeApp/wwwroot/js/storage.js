// `wwwroot/js/storage.js`
// Simple storage helpers for JS interop from .NET (BlazorWebView).

window.getFromStorage = (key) => {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
};

window.setToStorage = (key, value) => {
  try {
    localStorage.setItem(key, value);
  } catch {
    // ignore
  }
};

window.removeFromStorage = (key) => {
  try {
    localStorage.removeItem(key);
  } catch {
    // ignore
  }
};
                        