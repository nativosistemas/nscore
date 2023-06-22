window.addEventListener("load", (event) => {
    console.log("page is fully loaded");
    //$("#spinner").hide();
    document.getElementById("spinner").style.display = "none";
    fetchStarsJSON().then(stars => {
        var strHtml = '';
        //stars; // fetched movies

        stars.forEach(element => {
            if (element.name != null && element.name != '') {
                var disabled = '';
                if (!element.visible) {
                    disabled = ' disabled list-group-item-dark ';
                }
                strHtml += '<li class="list-group-item' + disabled + '" value="' + element.nameBayer + '">' + element.name  +  '</li>';
            }
        }
        );
        // document.getElementById("datalistOptionsStar").innerHTML = strHtml;
        document.getElementById("miLista").innerHTML = strHtml;

        // Obtén una referencia a la lista de elementos <li>
        var elementosLi = document.querySelectorAll("#miLista li");

        // Asigna un controlador de eventos a cada elemento <li>
        elementosLi.forEach(function (li) {
            li.addEventListener("click", function (event) {
                capturarEvento(li);
            });
        });
    });




});
function quitarActiveLi() {

    var elementosLi = document.querySelectorAll("#miLista li");

    // Asigna un controlador de eventos a cada elemento <li>
    elementosLi.forEach(function (li) {
        li.classList.remove("active");
    });
}
// Función para capturar el evento
function capturarEvento(elemento) {
    quitarActiveLi();
    elemento.classList.add("active");
    onClickStar(elemento.value);
    //  alert("Se hizo clic en el elemento: " + elemento.textContent + " id: " + elemento.value);
    // Aquí puedes realizar las acciones que desees al capturar el evento
}
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
/*function onClickStar() {
    if (!isOnClickStar) {
        isOnClickStar = true;
        // $("#spinner").show();
        document.getElementById("spinner").style.display = '';
        var id = document.getElementById("starDataList").value;
        fetchServo(id).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickStar = false;
            document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
        });
    }
}*/
function onClickStar(pId) {
    if (!isOnClickStar) {
        isOnClickStar = true;
        // $("#spinner").show();
        document.getElementById("spinner").style.display = '';
        var id = pId;
        fetchServo(id).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickStar = false;
            document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
        });
    }
}