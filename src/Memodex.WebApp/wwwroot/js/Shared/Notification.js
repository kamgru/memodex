"use strict";

function showNotification(type, message) {
    var _a;
    const notificationPopup = document.getElementById('notificationPopup');
    const notificationMessage = document.getElementById('notificationMessage');
    if (notificationPopup && notificationMessage) {
        notificationMessage.textContent = message;
        notificationPopup.classList.remove('hidden');
        if (type === 'Error') {
            notificationPopup.classList.add('error');
        } else if (type === 'Success') {
            notificationPopup.classList.add('success');
        }
        (_a = notificationPopup.querySelector('.close-popup')) === null || _a === void 0 ? void 0 : _a.addEventListener('click', () => {
            notificationPopup.classList.add('hidden');
        });
    }
}

const notificationElement = document.getElementById('notification');
const message = notificationElement === null || notificationElement === void 0 ? void 0 : notificationElement.getAttribute('data-message');
const notificationType = notificationElement === null || notificationElement === void 0 ? void 0 : notificationElement.getAttribute('data-notification-type');
if (notificationType && message) {
    showNotification(notificationType, message);
}
