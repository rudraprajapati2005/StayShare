(function(){
	// Smooth scroll for in-page links (if any future anchors exist)
	document.querySelectorAll('a[href^="#"]').forEach(a => {
		a.addEventListener('click', (e) => {
			const id = a.getAttribute('href');
			if (id && id.length > 1) {
				e.preventDefault();
				document.querySelector(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
			}
		});
	});

	// Card hover elevate is handled via CSS; this file is kept minimal for now
})();

