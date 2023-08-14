let darkModeBtn = document.querySelector('#dark-mode-btn');
darkModeBtn.addEventListener('click', (e) => {
    e.preventDefault();

    document.documentElement.classList.toggle('dark');
    const currentTheme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
    localStorage.setItem('theme', currentTheme);
    updateTheme();
});

const updateTheme = () => {
    const newTheme = localStorage.getItem('theme') === 'dark' ? 'dark' : 'light';
    const profileId = parseInt(document.documentElement.getAttribute('data-profile-id'));

    if (!profileId) {
        return;
    }
    const token = document.querySelector('meta[name="AntiForgeryToken"]').content;
    
    const url = '/myprofile?handler=updatetheme';
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({profileId: profileId, theme: newTheme})
    })
        .then(response => response.json());
}

document.querySelector('.relative.inline-block').addEventListener('click', function() {
    const dropdown = document.getElementById('profileDropdown');
    if (dropdown.classList.contains('hidden')) {
        dropdown.classList.remove('hidden');
    } else {
        dropdown.classList.add('hidden');
    }
});
