window.scrollToBottom = (element) => {
    if (element && element instanceof HTMLElement) {
        element.scrollTop = element.scrollHeight;
    }
};
