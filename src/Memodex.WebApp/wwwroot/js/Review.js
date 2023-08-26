"use strict";
class Review {
    init() {
        const revealBtn = document.querySelector('#reveal-btn');
        const hideBtn = document.querySelector('#hide-btn');
        const answer = document.querySelector('#answer');
        revealBtn === null || revealBtn === void 0 ? void 0 : revealBtn.addEventListener('click', (e) => {
            e.preventDefault();
            answer.classList.toggle('hidden');
            revealBtn.classList.toggle('hidden');
            hideBtn.classList.toggle('hidden');
        });
        hideBtn.addEventListener('click', (e) => {
            e.preventDefault();
            answer.classList.toggle('hidden');
            revealBtn === null || revealBtn === void 0 ? void 0 : revealBtn.classList.toggle('hidden');
            hideBtn.classList.toggle('hidden');
        });
    }
}
new Review().init();
