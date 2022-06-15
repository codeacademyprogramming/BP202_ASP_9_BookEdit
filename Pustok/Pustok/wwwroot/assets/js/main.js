$(document).ready(function () {

    $(".show-detail").click(function (e) {
        e.preventDefault();
        let url = $(this).attr("href");

        fetch(url).then(response => {
            if (!response.ok) {
                alert("Xeta bas verdi")
                return;
            }
           return response.text()
        })
            .then(data => {
                if (data) {
                    $("#bookDetailModal .modal-content").html(data);
                    $("#bookDetailModal").modal('show');
                }
                
            })

       
    })
})