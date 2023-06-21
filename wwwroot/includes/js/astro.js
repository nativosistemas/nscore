window.addEventListener("load", (event) => {
    console.log("page is fully loaded");
    //$("#spinner").hide();
    document.getElementById("spinner").style.display = "none";
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
       // $("#spinner").show();
        document.getElementById("spinner").style.display ='';
        var id =  document.getElementById("starDataList").value;
        fetchServo(id).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickStar = false;
            document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
        });
    }
}