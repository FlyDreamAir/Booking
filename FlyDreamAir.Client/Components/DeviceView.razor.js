window.DeviceView = (function () {
    let matchMedia = window.matchMedia("only screen and (max-width: 768px)");
    let components = new Set();

    matchMedia.addEventListener("change", function () {
        components.forEach(function (c) {
            c.invokeMethodAsync("OnChange", matchMedia.matches);
        });
    });

    async function registerAsync(c) {
        if (!components.has(c)) {
            components.add(c);
            await c.invokeMethodAsync("OnChange", matchMedia.matches);
        }
    }

    function unregister(c) {
        components.delete(c);
    }

    function isMobile() {
        return matchMedia.matches;
    }

    return {
        RegisterAsync: registerAsync,
        Unregister: unregister,
        IsMobile: isMobile
    }
})();
