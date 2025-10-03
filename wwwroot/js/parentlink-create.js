(function () {
	const emailInput = document.querySelector('input[name="parentEmail"]');
	if (!emailInput) return;
	const msgSpan = document.querySelector('[data-valmsg-for="parentEmail"]');
	let timer;
	async function checkEmail(value){
		if (!value || value.trim().length === 0){ if(msgSpan){ msgSpan.textContent=''; } return; }
		try{
			const res = await fetch(`/ParentLink/CheckGuardian?email=${encodeURIComponent(value.trim())}`);
			const data = await res.json();
			if (!data.exists){ if(msgSpan){ msgSpan.textContent = 'No account found for this email'; } return; }
			if (data.exists && !data.roleOk){ if(msgSpan){ msgSpan.textContent = 'This user is not a Guardian'; } return; }
			if (msgSpan){ msgSpan.textContent = `Found: ${data.name}`; msgSpan.classList.remove('text-danger'); }
		}catch(e){ /* ignore */ }
	}
	emailInput.addEventListener('input', function(){
		clearTimeout(timer);
		timer = setTimeout(() => checkEmail(emailInput.value), 350);
	});
})();


