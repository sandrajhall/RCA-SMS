window.menuInterop = {
    // Function to get the current window width
    getWindowWidth: function () {
        return window.innerWidth;
    },

    // Function to initialize a window resize listener
    // This listener will invoke a Blazor method when the window is resized
    // dotnetHelper: A reference to the .NET component that exposes the InvokeVoidAsync method
    // methodName: The name of the Blazor method to call on resize
    setupResizeListener: function (dotnetHelper, methodName) {
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                dotnetHelper.invokeMethodAsync(methodName, window.innerWidth);
            }, 200); // Debounce for better performance
        });
    },

    // Function to collapse the menu
    collapseMenu: function (elementId) {
        var menu = document.getElementById(elementId);
        if (menu && menu.classList.contains("expanded")) {
            menu.classList.remove("expanded");
            menu.classList.add("collapsed");
        }
    },

    // Function to expand the menu
    expandMenu: function (elementId) {
        var menu = document.getElementById(elementId);
        if (menu && menu.classList.contains("collapsed")) {
            menu.classList.remove("collapsed");
            menu.classList.add("expanded");
        }
    }
};
