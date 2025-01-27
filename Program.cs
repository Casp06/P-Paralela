using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Pedido
{
    public int Id { get; set; }
    public string Estado { get; set; }
    public int Prioridad { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Iniciando simulación de procesamiento de pedidos...");

        // Crear pedidos
        List<Pedido> pedidos = new List<Pedido>
        {
            new Pedido { Id = 1, Estado = "Pendiente", Prioridad = 1 },
            new Pedido { Id = 2, Estado = "Pendiente", Prioridad = 2 },
            new Pedido { Id = 3, Estado = "Pendiente", Prioridad = 1 }
        };

        // Procesar los pedidos en paralelo
        var tareas = pedidos.Select(p => ProcesarPedidoAsync(p)).ToList();

        while (tareas.Any())
        {
            // Esperar a que el primer pedido termine
            Task<Pedido> tareaCompletada = await Task.WhenAny(tareas);
            tareas.Remove(tareaCompletada);

            Pedido pedidoProcesado = await tareaCompletada;
            Console.WriteLine($"Pedido {pedidoProcesado.Id} completado con estado: {pedidoProcesado.Estado}");
        }

        Console.WriteLine("Simulación finalizada.");
    }

    static async Task<Pedido> ProcesarPedidoAsync(Pedido pedido)
    {
        Console.WriteLine($"[Pedido {pedido.Id}] Iniciando procesamiento...");

        try
        {
            await ValidarPedido(pedido);
            await AutorizarPago(pedido)
                .ContinueWith(async t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        await EmpacarPedido(pedido);
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            pedido.Estado = "Completado";
        }
        catch (Exception ex)
        {
            pedido.Estado = "Error: " + ex.Message;
        }

        return pedido;
    }

    static async Task ValidarPedido(Pedido pedido)
    {
        Console.WriteLine($"[Pedido {pedido.Id}] Validando...");
        await Task.Delay(new Random().Next(1000, 3000));
        Console.WriteLine($"[Pedido {pedido.Id}] Validación completada.");
    }

    static async Task AutorizarPago(Pedido pedido)
    {
        Console.WriteLine($"[Pedido {pedido.Id}] Autorizando pago...");
        await Task.Delay(2000);

        if (new Random().Next(0, 100) < 20) // 20% de probabilidad de fallo
        {
            throw new Exception("Pago rechazado");
        }

        Console.WriteLine($"[Pedido {pedido.Id}] Pago autorizado.");
    }

    static async Task EmpacarPedido(Pedido pedido)
    {
        Console.WriteLine($"[Pedido {pedido.Id}] Empacando...");
        await Task.Delay(2000);
        Console.WriteLine($"[Pedido {pedido.Id}] Empaque completado.");
    }
}
