# Proyecto2_ED2
Segundo proyecto del curso Estructura de Datos II

# Guate Envíos - Sistema de Enrutamiento Logístico 🇬🇹

Proyecto final para el curso de **Estructura de Datos II**. Es un sistema logístico avanzado para la gestión de paquetería y optimización de rutas en Guatemala, desarrollado en C# bajo una arquitectura limpia y una interfaz gráfica interactiva en WPF.

##  Descripción del Proyecto y Escenario
**Guate Envíos** simula la red logística de una empresa de paquetería a nivel nacional. El escenario principal resuelve dos grandes desafíos de la industria:
1. **La Topología de la Red:** Mantener un mapa de las sedes (ciudades) y sus conexiones (carreteras), considerando factores como el tiempo de viaje, el costo y el estado operativo de las mismas (cierres o aislamientos).
2. **El Procesamiento de Envíos:** Procesar varios paquetes garantizando su unicidad mediante un código de rastreo, validando si es factible entregarlos, y calculando la hoja de ruta matemática más óptima según la prioridad del cliente (Menor Costo o Menor Tiempo).


##  Estructuras Utilizadas y Justificación

Para lograr la eficiencia del sistema, se implementaron las siguientes estructuras y algoritmos:

1. **Grafo Dirigido (Listas de Adyacencia):** 
   * Uso: Modela la red de ciudades y carreteras.
   * Justificación: La red vial es un grafo "disperso". Usar una Matriz de Adyacencia por lo que la Lista de Adyacencia ahorra memoria RAM a cambio de un ligero costo de tiempo en la búsqueda de vecinos.
2. **Tabla Hash (con Encadenamiento):**
   * Uso: Almacenamiento y búsqueda de paquetes (Envíos).
   * Justificación: Garantiza tiempos de búsqueda y registro de **O(1)**. Se implementó una función Hash basada en *djb2* y listas enlazadas manuales para resolver colisiones, junto con un algoritmo de rehashing automático para escalar el tamaño del arreglo.
3. **Árbol Trie (Árbol de Prefijos):**
   * Uso: Autocompletado de ciudades en la interfaz de usuario.
   * Justificación: Evita iterar sobre todo el arreglo de nodos usando `Contains()`. Busca coincidencias en tiempo **O(m)** (longitud de la palabra), haciendo que la interfaz gráfica sea fluida y responsiva.
4. **Union-Find (Conjuntos Disjuntos):**
   * Uso: Validación instantánea de conectividad entre ciudades.
   * Justificación: Evita realizar un costoso recorrido de grafos recursivo (DFS) cada vez que se intenta registrar un paquete. Valida en microsegundos si la Sede A puede llegar a la Sede B (con optimizaciones de Path Compression y Union by Rank).
5. **Algoritmos de Enrutamiento (Dijkstra, BFS, DFS):**
   * Dijkstra: Calcula la ruta óptima para el cliente según el peso elegido (Costo en Quetzales o Tiempo en Minutos).
   * BFS (Anchura): Calcula rutas logísticas con la menor cantidad de escalas (ignorando el peso), ideal para paquetes frágiles.
   * DFS (Profundidad): Permite realizar auditorías de red o trazar rutas de contingencia exhaustivas (ej. el camión repartidor visita todas las sedes posibles hasta llegar a un punto ciego).

##  Instrucciones de Uso y Ejecución

### Prerrequisitos:
* **IDE:** Visual Studio 2019, 2022 o superior.
* **Framework:** .NET Framework 4.7.2 (o compatible).
* Sistema Operativo Windows (para la correcta ejecución de WPF).

### Pasos para ejecutar:
1. Clonar este repositorio:
   
```bash
   git clone [https://github.com/Cris-Dl/Proyecto2_ED2.git](https://github.com/Cris-Dl/Proyecto2_ED2.git)

## DOCUMENTACION

[Manual Tecnico](https://github.com/Cris-Dl/Proyecto2_ED2/blob/main/Manual%20Tecnico%20Proyecto%20%232%20EDII.pdf)

[Manual De Usuario](https://www.canva.com/design/DAHaAzmiocw/emfsV6vbd3ERE_k7kJLv4Q/edit)
