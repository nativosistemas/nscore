var l_citys = [];
var city = null;
window.addEventListener("load", (event) => {
    document.getElementById("spinner").style.display = "none";

    var pagina = getNamePage();
    //|| pagina == 'index.html'

    if (pagina == 'estrellas.html') {

        loadIndex();

    } else if (pagina == 'config.html') {
        loadConfig();

    }
    else if (pagina == 'constelaciones.html') {
        loadConstelaciones();

    }
    else if (pagina == 'servos.html') {
        // document.getElementById("spinner").style.display = "none";

    } else if (pagina == 'espaciolab.html') { 
        loadIndex();
    }
    if (pagina == 'estrellas.html' || pagina == 'config.html' || pagina == 'constelaciones.html') {
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
    }
});
function getNamePage() {
    var url = location.href;
    var pagina = url.substring(url.lastIndexOf('/') + 1);
    return pagina;
}

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
    //document.getElementById("spinner").style.display = "none";
    fetchStarsJSON().then(stars => {
        var strHtml = '';
        //stars; // fetched movies

        stars.forEach(element => {
            if (element.name != null && element.name != ''  ) {//&& element.visible
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
function loadConstelaciones() {
    //$("#spinner").hide();
    //document.getElementById("spinner").style.display = "none";
    fetchConstellationsJSON().then(stars => {
        var strHtml = '';
        //stars; // fetched movies

        stars.forEach(element => {
            if (element.name != null && element.name != '' && element.visible) {
                var disabled = '';
                /*if (!element.visible) {
                    disabled = ' disabled list-group-item-dark ';
                }*/
                strHtml += '<li class="list-group-item' + disabled + '" value="' + element.id + '">' + element.name + ' (' + element.nameLatin + ')' + '</li>';
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
                capturarEvento_ConstellationsConstellations(li);
            });
        });
    });

}
function onClickVolver() {
    //window.location.href = "index.html";
    history.back();
    return false;
}
function onClickIrConfig() {
    window.location.href = "config.html";
    return false;
}
function onClickIrEstrellas() {
    window.location.href = "estrellas.html";
    return false;
}
function onClickIrConstelaciones() {
    window.location.href = "constelaciones.html";
    return false;
}
function onClickIrServos() {
    window.location.href = "servos.html";
    return false;
}
function onClickIrEspacioLab() {
    window.location.href = "espaciolab.html";
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
function capturarEvento_ConstellationsConstellations(elemento) {
    quitarActiveLi();
    elemento.classList.add("active");
    onClickConstellation(elemento.value);
}
function onClickConstellation(pId) {
    if (!isOnClickStar) {
        isOnClickStar = true;
        // $("#spinner").show();
        document.getElementById("spinner").style.display = '';
        var id = pId;
        fetchServoConstellation(id).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickStar = false;
            document.getElementById("spinner").style.display = "none";// $("#spinner").hide();
        });
    }
}
async function fetchStarsJSON() {
    const response = await fetch('/stars');
    const stars = await response.json();
    return stars;
}
async function fetchConstellationsJSON() {
    const response = await fetch('/constellations');
    const stars = await response.json();
    return stars;
}
async function fetchServo(pId) {
    const response = await fetch('/servo?id=' + pId);
    const text = await response.text();
    return text;
}
async function fetchServoConstellation(pId) {
    const response = await fetch('/servoconstellations?id=' + pId);
    const text = await response.text();
    return text;
}
function onClickApagarLaser() {
    fetchLaser(0, 0);
}
function onClickEncenderLaser() {
    fetchLaser(0, 1);
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
async function fetchSetServoMover(pH, pV, pH_min, pH_max, pV_min, pV_max, pOnLaser) {
    const response = await fetch('/servomover?pH=' + pH + '&pV=' + pV + '&pH_min=' + pH_min + '&pH_max=' + pH_max + '&pV_min=' + pV_min + '&pV_max=' + pV_max + '&pOnLaser=' + pOnLaser);
    const text = await response.text();
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
var isOnClickMoverServo = false;
function onClickMoverServo() {
    if (!isOnClickMoverServo) {
        isOnClickMoverServo = true;
        document.getElementById("spinner").style.display = '';
        var horizontal = 0;
        var vertical = 0;
        var horizontal_min = 0;
        var horizontal_max = 0;
        var vertical_min = 0;
        var vertical_max = 0;
        var laserOn = false;
        var inputElement_horizontal = document.getElementById('inputServoH');
        if (inputElement_horizontal.value.trim() !== '') {
            horizontal = inputElement_horizontal.value;
        }
        var inputElement_vertical = document.getElementById('inputServoV');
        if (inputElement_vertical.value.trim() !== '') {
            vertical = inputElement_vertical.value;
        }
        var inputElement_horizontal_min = document.getElementById('inputServoHmin');
        if (inputElement_horizontal_min.value.trim() !== '') {
            horizontal_min = inputElement_horizontal_min.value;
        }
        var inputElement_horizontal_max = document.getElementById('inputServoHmax');
        if (inputElement_horizontal_max.value.trim() !== '') {
            horizontal_max = inputElement_horizontal_max.value;
        }
        var inputElement_vertical_min = document.getElementById('inputServoVmin');
        if (inputElement_vertical_min.value.trim() !== '') {
            vertical_min = inputElement_vertical_min.value;
        }
        var inputElement_vertical_max = document.getElementById('inputServoVmax');
        if (inputElement_vertical_max.value.trim() !== '') {
            vertical_max = inputElement_vertical_max.value;
        }

        var checkboxLaserOn = document.getElementById('checkboxLaserOn');

        if (checkboxLaserOn.checked) {
            laserOn = true;
        }
        fetchSetServoMover(horizontal, vertical, horizontal_min, horizontal_max, vertical_min, vertical_max, laserOn).then(text => {
            var strHtml = '';
            strHtml += ' <div class="alert alert-primary" role="alert">' + text + '  </div>';
            document.getElementById("divMsg").innerHTML = strHtml;
            isOnClickMoverServo = false;
            document.getElementById("spinner").style.display = "none";

        })
    }
    return false;
}