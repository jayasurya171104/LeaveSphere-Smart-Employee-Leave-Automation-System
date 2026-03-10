// Leave date validation

document.addEventListener("DOMContentLoaded", function () {

    const fromDate = document.querySelector("input[name='FromDate']");
    const toDate = document.querySelector("input[name='ToDate']");

    if (fromDate && toDate) {
        toDate.addEventListener("change", function () {

            if (new Date(toDate.value) < new Date(fromDate.value)) {
                alert("To Date cannot be earlier than From Date");
                toDate.value = "";
            }
        });
    }
});