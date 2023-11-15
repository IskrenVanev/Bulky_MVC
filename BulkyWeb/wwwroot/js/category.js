$(document).ready(function () {
    // Function to handle category filter
    function filterBooks(category) {
        // Show all books initially
        $(".col-lg-3").hide();

        // Filter books based on the selected category
        if (category !== "all") {
            $(".col-lg-3[data-category='" + category + "']").show();
        } else {
            // Show all books if 'all' is selected
            $(".col-lg-3").show();
        }
    }

    // Event handler for category filter clicks
    $(".list-group-item").click(function () {
        // Remove 'active' class from all categories
        $(".list-group-item").removeClass("active text-white bg-primary");

        // Add 'active' class to the clicked category
        $(this).addClass("active text-white bg-primary");

        // Get the category from the clicked element's text
        var category = $(this).text().trim().toLowerCase();

        // Call the filterBooks function with the selected category
        filterBooks(category);
    });

    // Initialize the filter with the default category (all)
    filterBooks("all");
});