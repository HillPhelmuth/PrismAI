// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}
export function getLocation() {
  return new Promise((resolve, reject) => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        position => resolve(JSON.stringify(position.coords)),
        error => reject(error)
      );
    } else {
      reject(new Error('Geolocation is not supported by this browser.'));
    }
  });
}
export function scrollToTop() {
  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  });
}
