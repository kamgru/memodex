class ThemeToggle {

    public init() {
        const toggle = document.querySelector('#theme-toggle') as HTMLButtonElement;
        if (!toggle) {
            return;
        }

        toggle.addEventListener('click', this.handleToggleClick);
    }

    private handleToggleClick: EventListener = (e) => {
        e.preventDefault();

        document.documentElement.classList.toggle('dark');
        const currentTheme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        localStorage.setItem('theme', currentTheme);
        this.setTheme(currentTheme);
    }

    private setTheme(theme: string) {
        const tokenInput = document.querySelector('input[name=__RequestVerificationToken]') as HTMLInputElement;

        const url = '/MyProfile?handler=UpdateTheme';
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenInput.value
            },
            body: JSON.stringify({theme: theme})
        }).then();
    }
}

new ThemeToggle().init();
