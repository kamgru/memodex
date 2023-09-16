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
        const tokenInput = document.querySelector('input[name=__RequestVerificationToken]');
        const url = '/myprofile?handler=updatetheme';
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenInput.value
            },
            body: JSON.stringify({ theme: theme })
        }).then();
    }
}
new ThemeToggle().init();
