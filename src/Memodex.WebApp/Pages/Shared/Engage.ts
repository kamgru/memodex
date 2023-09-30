class Engage {
    public init() {
        const revealBtn = document.querySelector('#reveal-btn') as HTMLButtonElement;
        const hideBtn = document.querySelector('#hide-btn') as HTMLButtonElement;
        const answer = document.querySelector('#answer') as HTMLElement;

        revealBtn?.addEventListener('click', (e) => {
            e.preventDefault();
            answer.classList.toggle('hidden');
            revealBtn.classList.toggle('hidden');
            hideBtn.classList.toggle('hidden')
        });

        hideBtn?.addEventListener('click', (e) => {
            e.preventDefault();
            answer.classList.toggle('hidden');
            revealBtn?.classList.toggle('hidden');
            hideBtn.classList.toggle('hidden')
        });
    }
}

new Engage().init();

