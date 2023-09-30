class FlashcardEngage extends HTMLElement {

    private revealBtn: HTMLButtonElement;
    private hideBtn: HTMLButtonElement;
    private answer: HTMLElement;

    constructor() {
        super();

        this.revealBtn = document.querySelector('#reveal-btn') as HTMLButtonElement;
        this.hideBtn = document.querySelector('#hide-btn') as HTMLButtonElement;
        this.answer = document.querySelector('#answer') as HTMLElement;

        this.revealBtn?.addEventListener('click', (e) => {
            e.preventDefault();
            this.toggleAnswer();
        });

        this.hideBtn?.addEventListener('click', (e) => {
            e.preventDefault();
            this.toggleAnswer();
        });
    }

    public toggleAnswer() {
        this.answer.classList.toggle('hidden');
        this.revealBtn.classList.toggle('hidden');
        this.hideBtn.classList.toggle('hidden')
    }
}

customElements.define('flashcard-engage', FlashcardEngage);
