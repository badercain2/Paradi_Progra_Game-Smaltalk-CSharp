﻿using System;
using System.Collections.Generic;

namespace JuegoTablero
{
    public class Casilla
    {
        public string Propietario { get; set; }

        public Casilla()
        {
            Propietario = "Ninguno";
        }

        public void Marcar(string nombreJugador)
        {
            Propietario = nombreJugador;
        }
    }

    public class Tablero
    {
        public Casilla[,] Casillas { get; private set; }
        public int Tamaño { get; private set; }

        public Tablero(int tamaño)
        {
            Tamaño = tamaño;
            Casillas = new Casilla[Tamaño, Tamaño];
            InicializarTablero();
        }

        private void InicializarTablero()
        {
            for (int i = 0; i < Tamaño; i++)
            {
                for (int j = 0; j < Tamaño; j++)
                {
                    Casillas[i, j] = new Casilla();
                }
            }
        }

        public void MostrarTablero()
        {
            for (int i = 0; i < Tamaño; i++)
            {
                for (int j = 0; j < Tamaño; j++)
                {
                    if (Casillas[i, j].Propietario == "Ninguno")
                        Console.Write("( )\t");
                    else
                    {
                        string playerNumber = Casillas[i, j].Propietario.Split(' ')[1];
                        Console.Write($"(J{playerNumber})\t");
                    }
                }
                Console.WriteLine();
            }
        }

        public bool MoverJugador(int x, int y, string nombreJugador, Jugador jugador, List<Jugador> jugadores)
        {
            if (Casillas[x, y].Propietario != "Ninguno")
            {
                if (Casillas[x, y].Propietario != nombreJugador)
                {
                    Jugador oponente = jugadores.Find(j => j.Nombre == Casillas[x, y].Propietario);
                    if (oponente != null)
                    {
                        bool jugadorGanaDuelo = Duelo.IniciarDuelo(jugador, oponente);
                        if (jugadorGanaDuelo)
                        {
                            Console.WriteLine($"{nombreJugador} ha ganado el duelo y toma todas las casillas de {oponente.Nombre}!");
                            MarcarCasillasDeJugadorEliminado(oponente.Nombre, nombreJugador, jugador, jugadores);
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"{oponente.Nombre} ha ganado el duelo. {nombreJugador} ha sido eliminado del juego.");
                            jugador.CasillasPoseidas = 0;
                            jugador.PosicionesConquistadas.Clear();
                            return false;
                        }
                    }
                }
                return false; // La casilla ya está ocupada por el jugador actual
            }
            Casillas[x, y].Marcar(nombreJugador);
            jugador.CasillasPoseidas++;
            jugador.ActualizarPosicionesConquistadas(x, y);
            return true;
        }

        private void MarcarCasillasDeJugadorEliminado(string jugadorEliminado, string nuevoPropietario, Jugador nuevoJugador, List<Jugador> jugadores)
        {
            Jugador jugadorAEliminar = jugadores.Find(j => j.Nombre == jugadorEliminado);
            if (jugadorAEliminar != null)
            {
                foreach (var posicion in jugadorAEliminar.PosicionesConquistadas)
                {
                    Casillas[posicion.x, posicion.y].Marcar(nuevoPropietario);
                    nuevoJugador.CasillasPoseidas++;
                    nuevoJugador.ActualizarPosicionesConquistadas(posicion.x, posicion.y);
                }
                jugadorAEliminar.CasillasPoseidas = 0;
                jugadorAEliminar.PosicionesConquistadas.Clear();
                jugadores.Remove(jugadorAEliminar);
            }
        }
    }

    public class Jugador
    {
        public string Nombre { get; set; }
        public int CasillasPoseidas { get; set; }
        public List<(int x, int y)> PosicionesConquistadas { get; private set; }

        public Jugador(string nombre)
        {
            Nombre = nombre;
            CasillasPoseidas = 0;
            PosicionesConquistadas = new List<(int x, int y)>();
        }

        public bool MoverA(int nuevaX, int nuevaY, Tablero tablero, List<Jugador> jugadores)
        {
            if (nuevaX < 0 || nuevaX >= tablero.Tamaño || nuevaY < 0 || nuevaY >= tablero.Tamaño)
            {
                Console.WriteLine("Movimiento inválido. Las coordenadas están fuera del tablero.");
                return false;
            }

            if (CasillasPoseidas == 0 || EsMovimientoAdyacente(nuevaX, nuevaY))
            {
                return tablero.MoverJugador(nuevaX, nuevaY, Nombre, this, jugadores);
            }

            Console.WriteLine("Movimiento inválido. Solo puedes moverte a posiciones adyacentes a tus casillas conquistadas.");
            return false;
        }

        private bool EsMovimientoAdyacente(int x, int y)
        {
            foreach (var pos in PosicionesConquistadas)
            {
                int diffX = Math.Abs(x - pos.x);
                int diffY = Math.Abs(y - pos.y);
                if (diffX <= 1 && diffY <= 1 && !(diffX == 1 && diffY == 1))
                {
                    return true;
                }
            }
            return false;
        }

        public void ActualizarPosicionesConquistadas(int x, int y)
        {
            if (!PosicionesConquistadas.Contains((x, y)))
            {
                PosicionesConquistadas.Add((x, y));
            }
        }

        public void MostrarCasillasDisponibles(Tablero tablero)
        {
            Console.WriteLine($"Casillas disponibles para {Nombre}:");
            if (CasillasPoseidas == 0)
            {
                Console.WriteLine("Puedes moverte a cualquier casilla vacía del tablero.");
                return;
            }

            HashSet<(int x, int y)> casillasDisponibles = new HashSet<(int x, int y)>();

            foreach (var pos in PosicionesConquistadas)
            {
                List<(int x, int y)> posiblesAdyacentes = new List<(int x, int y)>
                {
                    (pos.x - 1, pos.y), (pos.x + 1, pos.y), (pos.x, pos.y - 1), (pos.x, pos.y + 1)
                };

                foreach (var adyacente in posiblesAdyacentes)
                {
                    if (adyacente.x >= 0 && adyacente.x < tablero.Tamaño && adyacente.y >= 0 && adyacente.y < tablero.Tamaño)
                    {
                        casillasDisponibles.Add(adyacente);
                    }
                }
            }

            foreach (var casilla in casillasDisponibles)
            {
                string propietario = tablero.Casillas[casilla.x, casilla.y].Propietario;
                Console.WriteLine($"({casilla.x}, {casilla.y}) - {(propietario == "Ninguno" ? "Libre" : $"Ocupada por {propietario}")}");
            }
        }
    }

    public class Duelo
    {
        private static Random random = new Random();

        public static bool IniciarDuelo(Jugador jugador1, Jugador jugador2)
        {
            Console.WriteLine($"¡Duelo entre {jugador1.Nombre} y {jugador2.Nombre}!");
            
            int puntajeJugador1 = 0;
            int puntajeJugador2 = 0;
            
            for (int i = 0; i < 3; i++)
            {
                var (pregunta, respuestaCorrecta) = GenerarPregunta();
                Console.WriteLine($"Pregunta {i + 1}: {pregunta}");
                
                Console.WriteLine($"{jugador1.Nombre}, tu respuesta:");
                bool respuestaJugador1 = EvaluarRespuesta(Console.ReadLine(), respuestaCorrecta);
                
                Console.WriteLine($"{jugador2.Nombre}, tu respuesta:");
                bool respuestaJugador2 = EvaluarRespuesta(Console.ReadLine(), respuestaCorrecta);
                
                if (respuestaJugador1) puntajeJugador1++;
                if (respuestaJugador2) puntajeJugador2++;
            }
            
            Console.WriteLine($"Resultado del duelo: {jugador1.Nombre} {puntajeJugador1} - {puntajeJugador2} {jugador2.Nombre}");
            
            if (puntajeJugador1 > puntajeJugador2)
            {
                Console.WriteLine($"{jugador1.Nombre} gana el duelo!");
                return true;
            }
            else if (puntajeJugador2 > puntajeJugador1)
            {
                Console.WriteLine($"{jugador2.Nombre} gana el duelo!");
                return false;
            }
            else
            {
                Console.WriteLine("¡Empate! Se decidirá al azar.");
                return random.Next(2) == 0;
            }
        }

        private static (string pregunta, string respuestaCorrecta) GenerarPregunta()
        {
            string[][] preguntas = {
                new[] {"¿Cuál es la capital de Francia?", "Paris"},
                new[] {"¿En qué año comenzó la Segunda Guerra Mundial?", "1939"},
                new[] {"¿Cuál es el planeta más grande del sistema solar?", "Jupiter"},
                new[] {"¿Quién pintó la Mona Lisa?", "Leonardo da Vinci"},
                new[] {"¿Cuál es el elemento químico más abundante en el universo?", "Hidrogeno"}
            };
            string[] preguntaSeleccionada = preguntas[random.Next(preguntas.Length)];
            return (preguntaSeleccionada[0], preguntaSeleccionada[1]);
        }

        private static bool EvaluarRespuesta(string respuesta, string respuestaCorrecta)
        {
            return respuesta.Trim().Equals(respuestaCorrecta, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class Juego
    {
        private Tablero tablero;
        private List<Jugador> jugadores;

        public Juego(int tamañoTablero)
        {
            tablero = new Tablero(tamañoTablero);
            jugadores = new List<Jugador>();
            InicializarJugadores();
        }

        private void InicializarJugadores()
        {
            Console.WriteLine("¿Cuántos jugadores van a participar?");
            int numeroJugadores;
            while (!int.TryParse(Console.ReadLine(), out numeroJugadores) || numeroJugadores < 2)
            {
                Console.WriteLine("Por favor, ingrese un número válido de jugadores (mínimo 2):");
            }

            for (int i = 0; i < numeroJugadores; i++)
            {
                string nombreJugador = $"Jugador {i + 1}";
                jugadores.Add(new Jugador(nombreJugador));
            }
        }

        public void IniciarJuego()
        {
            while (jugadores.Count > 1)
            {
                for (int i = 0; i < jugadores.Count; i++)
                {
                    var jugador = jugadores[i];
                    bool movimientoValido = false;

                    while (!movimientoValido)
                    {
                        Console.Clear();
                        tablero.MostrarTablero();
                        Console.WriteLine($"Turno de {jugador.Nombre}");
                        Console.WriteLine($"Casillas poseídas por {jugador.Nombre}: {jugador.CasillasPoseidas}");

                        jugador.MostrarCasillasDisponibles(tablero);

                        Console.WriteLine($"{jugador.Nombre}, ingresa las coordenadas a donde te quieres mover (formato: x y): ");
                        string[] coordenadas = Console.ReadLine().Split(' ');

                        if (coordenadas.Length == 2 && int.TryParse(coordenadas[0], out int nuevaX) && int.TryParse(coordenadas[1], out int nuevaY))
                        {
                            movimientoValido = jugador.MoverA(nuevaX, nuevaY, tablero, jugadores);
                            if (!movimientoValido)
                            {
                                Console.WriteLine("Movimiento inválido. Intenta de nuevo.");
                                Console.ReadLine(); // Pausa para que el jugador pueda leer el mensaje
                            }
                        }
                        else
                        {
                            Console.WriteLine("Coordenadas inválidas. Inténtalo de nuevo.");
                            Console.ReadLine(); // Pausa para que el jugador pueda leer el mensaje
                        }
                    }

                    if (jugador.CasillasPoseidas == 0)
                    {
                        Console.WriteLine($"{jugador.Nombre} ha sido eliminado del juego.");
                        jugadores.RemoveAt(i);
                        i--;
                    }

                    Console.WriteLine("Presiona Enter para continuar al siguiente turno...");
                    Console.ReadLine();
                }
            }

            Console.Clear();
            tablero.MostrarTablero();
            Console.WriteLine($"¡{jugadores[0].Nombre} ha ganado el juego!");
        }
    }

    public class Programa
    {
        public static void Main(string[] args)
        {
            Juego juego = new Juego(4); // Crea un tablero 4x4
            juego.IniciarJuego();
        }
    }
}
