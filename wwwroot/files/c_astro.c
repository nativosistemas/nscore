#include <stdio.h>
#include <stdlib.h>

int main(int argc, char *argv[]) {
    if (argc < 4) {
        printf("Se esperaba al menos un argumento.\n");
        return 1;
    }

    float parametroH = atof(argv[1]);
    float parametroV = atof(argv[2]);
    int parametroLaser = atoi(argv[3]);
    float suma = parametroH + parametroV + parametroLaser;

    printf("Los parámetros recibidos son: parametroH: %.2f - parametroV: %.2f - parametroLaser: %d - suma: %.2f\n",
           parametroH, parametroV, parametroLaser, suma);

    return 0;
}