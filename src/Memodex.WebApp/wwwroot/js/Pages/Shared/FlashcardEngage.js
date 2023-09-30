"use strict";
class FlashcardEngage extends HTMLElement {
    constructor() {
        var _a, _b;
        super();
        this.revealBtn = document.querySelector('#reveal-btn');
        this.hideBtn = document.querySelector('#hide-btn');
        this.answer = document.querySelector('#answer');
        (_a = this.revealBtn) === null || _a === void 0 ? void 0 : _a.addEventListener('click', (e) => {
            e.preventDefault();
            this.toggleAnswer();
        });
        (_b = this.hideBtn) === null || _b === void 0 ? void 0 : _b.addEventListener('click', (e) => {
            e.preventDefault();
            this.toggleAnswer();
        });
    }
    toggleAnswer() {
        this.answer.classList.toggle('hidden');
        this.revealBtn.classList.toggle('hidden');
        this.hideBtn.classList.toggle('hidden');
    }
}
customElements.define('flashcard-engage', FlashcardEngage);
