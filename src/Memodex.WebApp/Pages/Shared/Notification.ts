function showNotification(type: string, message: string): void {
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

        notificationPopup.querySelector('.close-popup')?.addEventListener('click', () => {
            notificationPopup.classList.add('hidden');
        });
    }
}

const notificationElement = document.getElementById('notification');
const message = notificationElement?.getAttribute('data-message');
const notificationType = notificationElement?.getAttribute('data-notification-type');

if (notificationType && message) {
    showNotification(notificationType, message);
}
