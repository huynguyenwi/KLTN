$(function () {

    if ($("a.confirmDeletion").length) {
        $("a.confirmDeletion").click(() => {
            if (!confirm("Bạn có chắc chắn muốn xóa không?")) return false;
        });
    }

    if ($("div.alert.notification").length) {
        setTimeout(() => {
            $("div.alert.notification").fadeOut();
        }, 2000);
    }

});

function readURL(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();

        reader.onload = function (e) {
            $("img#imgpreview").attr("src", e.target.result).width(120).height(120);
        };

        reader.readAsDataURL(input.files[0]);
    }
}