document.getElementById('password').addEventListener('keyup', function(event) {
	// Check which form should be sumitted
	checkSubmittedForm(event);
});

document.getElementById('validate').addEventListener('keyup', function(event) {
	// Check which form should be sumitted
	checkSubmittedForm(event);
});

document.getElementById('login').addEventListener('click', function() {
	// Check if the password is filled
	let passwordElement = document.getElementById('password');

	if (passwordElement.value === undefined || passwordElement.value.length === 0) return;

	// Try to log into the account
	mp.trigger('requestPlayerLogin', passwordElement.value);
});

document.getElementById('register').addEventListener('click', function() {
	// Check if both passwords match
	let passwordElement = document.getElementById('password');
	let validateElement = document.getElementById('validate');

	if (passwordElement.value === undefined || validateElement.value === undefined) return;
	if (passwordElement.value.length === 0 || validateElement.value.length === 0) return;
	
	if (passwordElement.value !== validateElement.value) {
		// Show the error text
		return;
	}

	// Create a new account
	mp.events.call('createPlayerAccount', passwordElement.value);
});

function showLogin(socialName) {
	// Load login texts
	document.getElementById('header').innerText = i18next.t('connection.login-title');
	document.getElementById('social').value = socialName;
	document.getElementById('login').innerText = i18next.t('connection.login-button');
	document.getElementById('password').placeholder = i18next.t('connection.password');
	document.getElementById('description').innerText = i18next.t('connection.login-message');

	// Show the login button
	document.getElementById('login').classList.remove('no-display');

	// Focus on the password
	document.getElementById('password').focus();
}

function showRegister(socialName) {
	// Load register texts
	document.getElementById('header').innerText = i18next.t('connection.register-title');
	document.getElementById('social').value = socialName;
	document.getElementById('register').innerText = i18next.t('connection.register-button');
	document.getElementById('password').placeholder = i18next.t('connection.password');
	document.getElementById('validate').placeholder = i18next.t('connection.validate');
	document.getElementById('description').innerText = i18next.t('connection.register-message');

	// Show the password validation input and register button
	document.getElementById('register').classList.remove('no-display');
	document.getElementById('validate').parentElement.classList.remove('hidden');

	// Focus on the password
	document.getElementById('password').focus();
}

function checkSubmittedForm(event) {
	// Check the type of the form to be submitted
	submitForm(event, document.getElementById('login').classList.contains('no-display') ? 'register' : 'login');
}

function submitForm(event, clickable) {
	// Cancel the default action
	event.preventDefault();

	if (event.keyCode === 13) {
		// Submit the selected form
		document.getElementById(clickable).click();
	}
}
