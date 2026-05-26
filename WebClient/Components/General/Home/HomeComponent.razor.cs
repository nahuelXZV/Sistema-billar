
namespace WebClient.Components.General.Home;

public partial class HomeComponent
{
    private HomeDashboardViewModel? Dashboard { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dashboard = await LoadDashboardAsync();
    }

    private Task<HomeDashboardViewModel> LoadDashboardAsync()
    {
        // Temporary preview data for the home screen.
        // Replace this method later with service calls to your real API.
        return Task.FromResult(BuildPreviewDashboard());
    }

    private static HomeDashboardViewModel BuildPreviewDashboard()
    {
        return new HomeDashboardViewModel
        {
            GreetingTitle = "Panel principal del billar",
            GreetingDescription = "Controla mesas, reservas, ventas y alertas del turno desde un solo lugar.",
            TodayLabel = "Martes 26 de mayo",
            ActiveShift = "Noche",
            LiveStatus = "6 mesas activas y 2 reservas en espera",
            HighlightAmount = "$ 4.850",
            HighlightCaption = "Objetivo diario: $ 6.000",
            HighlightSubtext = "Faltan $ 1.150 para cerrar la meta del turno.",
            HighlightProgress = 81,
            TableSummaryLabel = "9 mesas monitoreadas",
            Metrics =
            [
                new DashboardMetric("Ventas del dia", "$ 4.850", "+18% vs ayer", "bi bi-cash-coin", "tone-success"),
                new DashboardMetric("Mesas ocupadas", "6 / 9", "2 por liberarse en menos de 30 min", "bi bi-grid-3x3-gap-fill", "tone-primary"),
                new DashboardMetric("Reservas pendientes", "4", "2 confirmadas y 2 por validar", "bi bi-calendar-check", "tone-warning"),
                new DashboardMetric("Stock critico", "3", "Snacks y bebidas por reponer", "bi bi-exclamation-diamond", "tone-danger")
            ],
            Tables =
            [
                new TableStatus("Mesa 01", "En juego", "Nahuel vs Martin", "55 min consumidos", "badge bg-success-subtle text-success-emphasis", "table-busy"),
                new TableStatus("Mesa 02", "Reservada", "Turno 20:30", "Cliente: Carla Rojas", "badge bg-warning-subtle text-warning-emphasis", "table-booked"),
                new TableStatus("Mesa 03", "Libre", "Lista para asignar", "Ultima limpieza hace 10 min", "badge bg-secondary-subtle text-secondary-emphasis", "table-free"),
                new TableStatus("Mesa VIP", "En juego", "Torneo amistoso", "2 h reservadas", "badge bg-success-subtle text-success-emphasis", "table-busy"),
                new TableStatus("Mesa 05", "Mantenimiento", "Cambio de paño", "Disponible en 45 min", "badge bg-danger-subtle text-danger-emphasis", "table-warning"),
                new TableStatus("Mesa 06", "Libre", "Disponible ahora", "Ideal para reservas rapidas", "badge bg-secondary-subtle text-secondary-emphasis", "table-free")
            ],
            UpcomingReservations =
            [
                new ReservationItem("20:30", "1 hora", "Carla Rojas", "Mesa 02 · 4 jugadores", "Confirmada", "badge bg-success-subtle text-success-emphasis"),
                new ReservationItem("21:00", "2 horas", "Equipo Norte", "Mesa VIP · Cumpleanos", "Por validar", "badge bg-warning-subtle text-warning-emphasis"),
                new ReservationItem("21:30", "1.5 horas", "Luis Arce", "Mesa 06 · Pareja", "Prepagada", "badge bg-info-subtle text-info-emphasis")
            ],
            Summaries =
            [
                new ProgressSummary("Ocupacion del salon", "67% del aforo operativo", 67, "tone-primary"),
                new ProgressSummary("Meta de ventas", "81% del objetivo diario", 81, "tone-success"),
                new ProgressSummary("Reservas confirmadas", "6 de 8 turnos cubiertos", 75, "tone-warning"),
                new ProgressSummary("Reposicion de barra", "2 pedidos en proceso", 40, "tone-danger")
            ],
            RecentActivities =
            [
                new ActivityItem("Se registro una nueva venta", "Combo Coca Cola + snack en barra", "Hace 8 min", "tone-success"),
                new ActivityItem("Mesa 03 finalizo su partida", "Quedo disponible para un nuevo turno", "Hace 14 min", "tone-primary"),
                new ActivityItem("Reserva pendiente de confirmar", "Equipo Norte para Mesa VIP", "Hace 19 min", "tone-warning"),
                new ActivityItem("Stock bajo detectado", "Papas clasicas y bebida energetica", "Hace 28 min", "tone-danger")
            ],
            Alerts =
            [
                new AlertItem("Productos por reponer", "La barra tiene 3 items con stock minimo. Conviene revisar inventario antes del siguiente turno fuerte.", "bi bi-box-seam", "tone-warning"),
                new AlertItem("Mesa en mantenimiento", "La Mesa 05 estara fuera de servicio mientras se termina el cambio de paño.", "bi bi-tools", "tone-danger"),
                new AlertItem("Turno premium en espera", "La reserva VIP de las 21:00 aun no fue confirmada por el cliente.", "bi bi-star", "tone-primary")
            ],
            QuickActions =
            [
                new QuickAction("Nueva reserva", "Registrar un turno manual para una mesa.", "/Mesas/Listado", "bi bi-calendar-plus", "tone-primary"),
                new QuickAction("Alta de producto", "Cargar bebidas, snacks o servicios nuevos.", "/Producto/Crear", "bi bi-cup-straw", "tone-success"),
                new QuickAction("Movimiento de stock", "Registrar entradas o salidas de inventario.", "/TransaccionInventario/Crear", "bi bi-arrow-left-right", "tone-warning"),
                new QuickAction("Gestion de clientes", "Revisar historial y contacto de jugadores.", "/Cliente/Listado", "bi bi-people", "tone-danger")
            ]
        };
    }

    private sealed class HomeDashboardViewModel
    {
        public string GreetingTitle { get; init; } = string.Empty;
        public string GreetingDescription { get; init; } = string.Empty;
        public string TodayLabel { get; init; } = string.Empty;
        public string ActiveShift { get; init; } = string.Empty;
        public string LiveStatus { get; init; } = string.Empty;
        public string HighlightAmount { get; init; } = string.Empty;
        public string HighlightCaption { get; init; } = string.Empty;
        public string HighlightSubtext { get; init; } = string.Empty;
        public int HighlightProgress { get; init; }
        public string TableSummaryLabel { get; init; } = string.Empty;
        public IReadOnlyList<DashboardMetric> Metrics { get; init; } = [];
        public IReadOnlyList<TableStatus> Tables { get; init; } = [];
        public IReadOnlyList<ReservationItem> UpcomingReservations { get; init; } = [];
        public IReadOnlyList<ProgressSummary> Summaries { get; init; } = [];
        public IReadOnlyList<ActivityItem> RecentActivities { get; init; } = [];
        public IReadOnlyList<AlertItem> Alerts { get; init; } = [];
        public IReadOnlyList<QuickAction> QuickActions { get; init; } = [];
    }

    private sealed record DashboardMetric(string Label, string Value, string Caption, string IconCss, string ToneClass);
    private sealed record TableStatus(string Name, string State, string PrimaryText, string SecondaryText, string BadgeClass, string StateClass);
    private sealed record ReservationItem(string Time, string Duration, string ClientName, string Detail, string Status, string BadgeClass);
    private sealed record ProgressSummary(string Label, string ValueText, int Progress, string ToneClass);
    private sealed record ActivityItem(string Title, string Description, string TimeLabel, string ToneClass);
    private sealed record AlertItem(string Title, string Description, string IconCss, string ToneClass);
    private sealed record QuickAction(string Title, string Description, string Url, string IconCss, string ToneClass);
}
