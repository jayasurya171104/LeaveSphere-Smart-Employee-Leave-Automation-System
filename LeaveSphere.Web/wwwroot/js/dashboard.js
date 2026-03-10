// Simple dashboard animation

document.addEventListener("DOMContentLoaded", function () {

    const cards = document.querySelectorAll(".card");

    cards.forEach(card => {
        card.classList.add("dashboard-card");
    });

});