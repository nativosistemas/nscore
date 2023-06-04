window.addEventListener("load", (event) => {
    console.log("page is fully loaded");

    fetchStarsJSON().then(stars => {
        var strHtml = '';
        //stars; // fetched movies

        stars.forEach(element => {
            strHtml += '<option value="' + element.nameBayer + '">' + element.name + '</option>';
            console.log(element);
        }
        );
        document.getElementById("datalistOptionsStar").innerHTML = strHtml;
    });

});


async function fetchStarsJSON() {
    const response = await fetch('/stars');
    const stars = await response.json();
    return stars;
}

async function fetchServo(pId) {
    const response = await fetch('/servo?id=' + pId);
    const text = await response.text();
    return text;
}
var isOnClickStar = false;
function onClickStar() {
    if (!isOnClickStar) {
        isOnClickStar = true;
        var id =  document.getElementById("starDataList").value;
        fetchServo(id).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickStar = false;
        });
    }
}