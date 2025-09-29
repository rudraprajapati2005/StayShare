document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    const submitBtn = form ? form.querySelector('button[type="submit"]') : null;
    
    if (!form || !submitBtn) {
        return; // Exit if elements not found
    }
    
    form.addEventListener('submit', function(e) {
        // Basic client-side validation
        const fullName = document.querySelector('input[name="FullName"]').value.trim();
        const gender = document.querySelector('select[name="Profile.Gender"]').value;
        const dateOfBirth = document.querySelector('input[name="Profile.DateOfBirth"]').value;
        const contactNumber = document.querySelector('input[name="Profile.ContactNumber"]').value;
        const maxBudget = document.querySelector('input[name="Profile.MaxBudget"]').value;
        
        let isValid = true;
        let errorMessage = '';
        
        if (!fullName) {
            errorMessage += 'Full name is required.\n';
            isValid = false;
        } else if (fullName.length > 100) {
            errorMessage += 'Full name cannot exceed 100 characters.\n';
            isValid = false;
        }
        
        // Email validation removed since email is read-only
        
        if (!gender) {
            errorMessage += 'Gender is required.\n';
            isValid = false;
        }
        
        if (!dateOfBirth) {
            errorMessage += 'Date of birth is required.\n';
            isValid = false;
        } else {
            const birthDate = new Date(dateOfBirth);
            const today = new Date();
            if (birthDate >= today) {
                errorMessage += 'Date of birth must be in the past.\n';
                isValid = false;
            }
        }
        
        if (!contactNumber) {
            errorMessage += 'Contact number is required.\n';
            isValid = false;
        }
        
        if (maxBudget && parseInt(maxBudget) < 0) {
            errorMessage += 'Budget cannot be negative.\n';
            isValid = false;
        }
        
        if (!isValid) {
            e.preventDefault();
            alert(errorMessage);
            return false;
        }
        
        // Disable submit button to prevent double submission
        submitBtn.disabled = true;
        submitBtn.textContent = 'Updating...';
    });
});
