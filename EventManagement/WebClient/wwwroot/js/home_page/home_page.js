$(document).ready(function () {
    $('.wishlist-btn').click(function () {
        $(this).toggleClass('active');
        let icon = $(this).find('i');
        if ($(this).hasClass('active')) {
            icon.removeClass('fa-regular').addClass('fa-solid text-danger');
        } else {
            icon.removeClass('fa-solid text-danger').addClass('fa-regular');
        }
    });
});
