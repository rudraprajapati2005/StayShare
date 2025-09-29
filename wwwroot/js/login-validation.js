document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('loginForm');
    const submitBtn = document.getElementById('submitBtn');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const emailError = document.getElementById('emailError');
    const passwordError = document.getElementById('passwordError');
    
    if (!form || !submitBtn || !emailInput || !passwordInput || !emailError || !passwordError) {
        return; // Exit if elements not found
    }
    
    // Real-time validation
    emailInput.addEventListener('blur', validateEmail);
    passwordInput.addEventListener('blur', validatePassword);
    
    function validateEmail() {
        const email = emailInput.value.trim();
        if (!email) {
            emailError.textContent = 'Email is required';
            return false;
        } else if (email.indexOf('@') === -1 || email.indexOf('.') === -1 || email.indexOf('@') === 0 || email.indexOf('@') === email.length - 1) {
            emailError.textContent = 'Please enter a valid email address';
            return false;
        } else {
            emailError.textContent = '';
            return true;
        }
    }
    
    function validatePassword() {
        const password = passwordInput.value;
        if (!password) {
            passwordError.textContent = 'Password is required';
            return false;
        } else {
            passwordError.textContent = '';
            return true;
        }
    }
    
    // Form submission validation
    form.addEventListener('submit', function(e) {
        const emailValid = validateEmail();
        const passwordValid = validatePassword();
        
        if (!emailValid || !passwordValid) {
            e.preventDefault();
            return false;
        }
        
        // Disable submit button to prevent double submission
        submitBtn.disabled = true;
        submitBtn.textContent = 'Signing In...';
    });
});
