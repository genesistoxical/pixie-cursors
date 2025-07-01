<p align="center"></p>
<p align="center"><a href="#"><img width="115px" src="docs/assets/Logo-115px.png" align="center" alt="Pixie Cursors"/></a></p>
<h1 align="center">Pixie Cursors</h1>
<p align="center">Pixie Cursors es una app para crear tus propios cursores personalizados en Windows.</p>

<p align="center">
 <a href="LICENSE"><img alt="License" src="https://img.shields.io/badge/License-MIT-FD2A7A?style=flat-square&labelColor=343B45"/></a>
 <a href="https://github.com/genesistoxical/pixie-cursors/releases/latest"><img src="https://img.shields.io/github/v/release/genesistoxical/pixie-cursors.svg?color=2AB4FC&label=Release&style=flat-square&labelColor=343B45"/></a>
 <a href="#"><img alt="NET" src="https://img.shields.io/badge/.NET_Framework-4.8-FD2A7A?style=flat-square&labelColor=343B45"/></a> 
 <a href="/installer%20src"><img alt="Installer" src="https://img.shields.io/badge/Installer-ISS-2AB4FC?style=flat-square&labelColor=343B45"/></a>
 <a href="#"><img alt="Languages" src="https://img.shields.io/badge/Languages-2-FD2A7A?style=flat-square&labelColor=343B45"/></a>
</p>

<p align="center">
<a href="README.md">English</a> :speech_balloon: <a href="README-es.md">Español</a>
</p>

## Características
* Crea cursores en un par de minutos.
* Interfaz intuitiva y sencilla.
* Vista previa en tiempo y tamaño real.
* Importa una imagen en PNG y recorta píxeles vacíos.
* Elige en dónde hará clic el cursor (Hotspot).
* Selector de color que permite guardar muestras.
* Botón para agregar sombra al finalizar el cursor.
* El fondo a cuadros del editor se puede oscurecer.
* Accede a las herramientas con accesos del teclado.
* Cambia entre el idioma Inglés y Español.

## Preview
<a href="#"><img src="docs/assets/Pixie-Cursors-App.png"/></a>

## Uso
El uso es muy sencillo, ya que es un editor como cualquier otro. Si tienes duda sobre una herramienta o botón, deja tu cursor encima de (sin dar clic) para obtener información.

- Abre una imagen ![Open](docs/assets/buttons/Open.png) (debe ser en formato png). Los márgenes con pixeles vacíos se recortarán automáticamente.
- Comienza a crear la flechita; utiliza el pincel, selector de color y tu creatividad. Al seleccionar el pincel, puedes borrar con clic derecho también.
- Una vez terminado, podrías agregar sombra ![Shadow](docs/assets/buttons/Shadow.png). Suele agregarse bastante sombreado a la flechita así que puedes perfeccionarla borrando un poco.
- Selecciona ![Hotspot](docs/assets/buttons/_Hotspot.png) de la barra de herramientas para elegir en dónde quieres que haga clic tu cursor, como por ejemplo, la punta de la flechita. Haz clic para dejar el circulito en el lugar que desees.
- Presiona el botón de guardar ![Save](docs/assets/buttons/Save.png), ponle un nombre y ya lo habrás terminado. Ahora vamos a instalarlo...

**Instalación:**
- Abre tu carpeta de cursores con el botón ![CursorsFolder](docs/assets/buttons/CursorsFolder.png) y mueve ahí el cursor que acabas de crear.
- Da clic en ![MouseProperties](docs/assets/buttons/MouseProperties.png) el cual es solo un acceso directo a las Propiedades del Mouse de Windows.
- En la ventanita que se abrirá, dirígete a punteros, clic en **Selección normal** y `Examinar` para elegir uno diferente. Entra a la carpeta de **- Pixie Cursors -** y selecciona tu cursor. Puedes hacer lo mismo con el cursor de link o manita.
- Presiona `Guardar como…`, ponle un nombre y `Aceptar`.
- Desactiva la casilla de **Habilitar sombra en el puntero** para que no se ponga doble sombra, da en `Aceptar` ¡Y listo, disfruta tu cursor!

**Otras herramientas:**
- Si está activado **Cortar Margen Vacío** al momento de abrir una imagen, este va a recortarse para otener el mejor tamaño del cursor.
- Oprime `Shift` al usar el pixel para hacer una línea.
- Oprime `Ctrl` al usar el bote de pintura o **Rellenar** para pintar todos los pixeles del mismo color.

<br>
<a href="#"><img src="docs/assets/Pixie-Cursors-Usage.gif"/></a>

<br></br>

Dando click en ![Info](docs/assets/buttons/Info.png), aparecerá una nueva ventana en la que puedes:

- **Cambiar el idioma:** En la sección inferior **Idioma**, haz clic en el icono de las flechitas para cambiar entre English, Español o Deutsch. Por último da clic en el botón de regresar para aplicar y ver los cambios.

- **Contenido de terceros:** En la parte izquierda están los nombres de cada librería, proyecto, iconos o tipografías que fueron utilizados, cliquea uno para leer su licencia y autor(a) o autores(as), además de una corta descripción. Puedes hacer clic sobre el icono de clip para ir al repositorio/sitio oficial de cada uno y cambiar de página con **Siguiente 🢖🢖** o **🢔🢔 Atrás**.

## Instalador
Para compilar el instalador es necesario [Inno Setup](https://jrsoftware.org/isinfo.php), los archivos se encuentran en la carpeta [installer src](/installer%20src). Solo debes abrir el proyecto (Installer.iss) y compilarlo, a menos que desees hacer una modificación. Al finalizar, en la misma ubicación dejará una carpeta llamada Output.

>* Por favor, toma en cuenta que primero debes compilar el código de fuente en Visual Studio para tener los binarios, de lo contrario obtendrás un [📍Compiler Error](https://github.com/genesistoxical/drop-icons/issues/3).
>* Si lo prefieres, puedes descargar la app compilada en la [página](https://genesistoxical.github.io/pixie-foders/) de Pixie Folders.

**Portable with 7zip.bat** permite comprimir en .zip rápidamente la versión portable. Únicamente funciona con [7zip](https://www.7-zip.org/).

.iss Obtiene los archivos que son necesarios dentro de su carpeta y/o la carpeta Release (debido a las rutas relativas).

## Contribuciones
* Si haces comentarios en el código, preferiblemente en Español, por favor.
* Los nombres de las variables deben estar en Inglés.
* Si abres un **Issue**, puede ser en Inglés o Español.
* **Pull request** en Inglés, en la descripción puedes agregar detalles en Inglés o Español.
  
## Configuración
El archivo `Config.ini` almacena información del lenguaje.

~~~
[Options]
Language = en
~~~

## Créditos
Pixie Cursors está basado en [PixelArtTool](https://github.com/unitycoder/PixelArtTool) bajo [MIT license](https://github.com/unitycoder/PixelArtTool/blob/master/LICENSE).

* [HandyControls](https://github.com/ghost1372/HandyControls) bajo [MIT License](https://github.com/ghost1372/HandyControls/blob/develop/LICENSE).

* [FolderBrowserEx](https://github.com/evaristocuesta/FolderBrowserEx) library bajo [MIT License](https://github.com/evaristocuesta/FolderBrowserEx/blob/master/LICENSE).

* Icons are part of [Teenyicons](https://github.com/teenyicons/teenyicons) bajo [MIT License](https://github.com/teenyicons/teenyicons/blob/master/LICENSE).

* [LazZiya.ImageResize](https://github.com/LazZiya/ImageResize) bajo [MIT License](https://github.com/LazZiya/ImageResize/blob/master/LICENSE).
<br><sub>Este paquete incluye una dependencia, lee más detalles en este [archivo](/src/PixieCursors/Docs/LazZiyaImageResize%20%2B.txt).</sup>

* [Noto Music](https://fonts.google.com/noto/specimen/Noto+Music) bajo [SIL Open Font License](/src/PixieCursors/Docs/Noto%20Music/OFL.txt).

**Puedes encontrar todas las licencias [aquí](/src/PixieCursors/Docs).*

## ¿Qué es lo nuevo?
`Versión 1.1.0` al abrir una imagen menor a 32px que no es cuadrada, ya no se ve borrosa.

`Versión 1.4.1` herramienta borrador arreglada, no funcionaba. Se ha mejorado el mensaje que se muestra al abrir una imagen cuando ya hay otra abierta.

## Licencia
**MIT License**

Copyright (c) 2025 Génesis Toxical ([read here](LICENSE)).

<br>

## Related:
`❤️ Crystal Folders` Personaliza los iconos de tus carpetas en segundos: [`Descarga`](https://genesistoxical.github.io/crystal-folders/) o [`Repositorio`](https://github.com/genesistoxical/crystal-folders).

`❤️ Custom Folder` Crea tu propia carpeta personalizada: [`genesistoxical.github.io/customfolder`](https://genesistoxical.github.io/customfolder/) o [`Repository`](https://github.com/genesistoxical/customfolder).

`❤️ Drop Icons` Utilidad para convertir imágenes a iconos (.ico) para Windows: [`Descarga`](https://genesistoxical.github.io/drop-icons/) o [`Repositorio`](https://github.com/genesistoxical/drop-icons/).

`❤️ Image to Icon` Convertir imagen a icono online: [`genesistoxical.github.io/imagetoicon`](https://genesistoxical.github.io/imagetoicon/) o [`Repositorio`](https://github.com/genesistoxical/imagetoicon).
