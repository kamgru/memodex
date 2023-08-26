"use strict";
class ThemeToggle {
    constructor() {
        this.handleToggleClick = (e) => {
            e.preventDefault();
            document.documentElement.classList.toggle('dark');
            const currentTheme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
            localStorage.setItem('theme', currentTheme);
            this.setTheme(currentTheme);
        };
    }
    init() {
        const toggle = document.querySelector('#theme-toggle');
        if (!toggle) {
            return;
        }
        toggle.addEventListener('click', this.handleToggleClick);
    }
    setTheme(theme) {
        var _a;
        const profileIdAttribute = document.documentElement.getAttribute('data-profile-id');
        if (!profileIdAttribute) {
            showNotification('Error', 'Unable to set theme.  Please refresh the page and try again.');
            return;
        }
        const profileId = parseInt(profileIdAttribute);
        if (!profileId) {
            showNotification('Error', 'Unable to set theme.  Please refresh the page and try again.');
            return;
        }
        const token = (_a = document.querySelector('meta[name="AntiForgeryToken"]')) === null || _a === void 0 ? void 0 : _a.getAttribute('content');
        if (!token) {
            showNotification('Error', 'Unable to set theme.  Please refresh the page and try again.');
            return;
        }
        const url = '/myprofile?handler=updatetheme';
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ profileId: profileId, theme: theme })
        });
    }
}
new ThemeToggle().init();
