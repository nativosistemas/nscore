var l_citys = [];
var city = null;
window.addEventListener("load", (event) => {
    console.log("page is fully loaded");

    loadConfig();
    loadIndex();
    fetchGetCity().then(el_city => {
        city = el_city;
    }).then(c => {

        if (document.getElementById("divMsgHead") !== null && city != null) {
            document.getElementById("divMsgHead").innerHTML = 'Lugar: ' + city.name;
        }
        if (document.getElementById("miCombo") !== null) {
            let element = document.getElementById("miCombo");
            element.value = city.id;
        }
    });
});

function onchangeCity() {
    //result.textContent = `You like ${event.target.value}`;
    var city_aux = l_citys.find(checkValue);
    if (city_aux != null) {
        fetchSetCity(city_aux.id, city_aux.name, String(city_aux.latitude), String(city_aux.longitude)).then(x => {
            city = x;
        });
    }
};
function checkValue(cmb) {
    return cmb.id == document.getElementById("miCombo").value;
}

function loadConfig() {
    fetchGetCitys().then(el_city => {
        var strHtml = '';
        l_citys = el_city;
        if (l_citys != null && l_citys.length > 0) {
            /*if (city == null) {
                city = l_citys[0];
            }*/
            const select = document.querySelector('select');
            l_citys.forEach(element => {
                strHtml += '<option  class="list-group-item" value="' + element.id + '">' + element.name + '</option >';
            }
            );
            if (document.getElementById("miCombo") !== null) {
                document.getElementById("miCombo").innerHTML = strHtml;
            }
        }

    })
}
function loadIndex() {
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
                strHtml += '<li class="list-group-item' + disabled + '" value="' + element.id + '">' + element.name + '</li>';
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

}
function onClickIrIndex() {
    window.location.href = "index.html";
    return false;
}
function onClickIrConfig() {
    window.location.href = "config.html";
    return false;
}
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
function onClickApagarLaser() {
    fetchLaser(0, 0);
}
async function fetchLaser(pRead, pOn) {
    const response = await fetch('/laser?read=' + pRead + '&on=' + pOn);
    const text = await response.text();
    return text;
}

async function fetchGetCitys() {
    const response = await fetch('/citys');
    const text = await response.json();
    return text;
}
async function fetchGetCity() {
    const response = await fetch('/getcity');
    const text = await response.json();
    return text;
}
async function fetchSetCity(pId, pName, pLat, pLon) {// string name, double lat, double lon
    const response = await fetch('/setcity?id=' + pId + '&name=' + pName + '&lat=' + pLat + '&lon=' + pLon);
    const text = await response.json();
    return text;
}
var isOnClickStar = false;

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