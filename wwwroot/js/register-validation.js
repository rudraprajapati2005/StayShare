document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('registerForm');
    const submitBtn = document.getElementById('submitBtn');
    
    if (!form || !submitBtn) return;
    
    // Client-side validation
    form.addEventListener('submit', function(e) {
        const fullName = document.querySelector('input[name="FullName"]').value.trim();
        const email = document.querySelector('input[name="Email"]').value.trim();
        const password = document.querySelector('input[name="PasswordHash"]').value;
        const role = document.querySelector('input[name="Role"]:checked');
        
        let isValid = true;
        let errorMessage = '';
        
        if (!fullName) {
            errorMessage += 'Full name is required.\n';
            isValid = false;
        }
        
        if (!email) {
            errorMessage += 'Email is required.\n';
            isValid = false;
        } else if (!email.includes('@') || !email.includes('.') || email.indexOf('@') === 0 || email.indexOf('@') === email.length - 1) {
            errorMessage += 'Please enter a valid email address.\n';
            isValid = false;
        }
        
        if (!password) {
            errorMessage += 'Password is required.\n';
            isValid = false;
        } else if (password.length < 6) {
            errorMessage += 'Password must be at least 6 characters long.\n';
            isValid = false;
        }
        
        if (!role) {
            errorMessage += 'Please select a role.\n';
            isValid = false;
        }
        
        if (!isValid) {
            e.preventDefault();
            alert(errorMessage);
            return false;
        }
        
        // Disable submit button to prevent double submission
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating Account...';
    });
});