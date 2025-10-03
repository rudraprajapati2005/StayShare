(function(){
	// Collapse/expand sections
	function toggleSection(btn){
		const target = document.querySelector(btn.getAttribute('data-target'));
		if(!target) return;
		target.style.display = (target.style.display === 'none') ? '' : 'none';
		btn.querySelector('i')?.classList.toggle('fa-chevron-up');
		btn.querySelector('i')?.classList.toggle('fa-chevron-down');
	}

	document.querySelectorAll('[data-toggle="pl-section"]').forEach(btn => {
		btn.addEventListener('click', () => toggleSection(btn));
	});

	// Subtle reveal animation for cards
	const cards = document.querySelectorAll('.pl-card');
	cards.forEach((c, idx) => {
		c.style.opacity = 0;
		c.style.transform = 'translateY(6px)';
		setTimeout(() => {
			c.style.transition = 'opacity .25s ease, transform .25s ease';
			c.style.opacity = 1;
			c.style.transform = 'translateY(0)';
		}, 50 + idx*40);
	});
})();



