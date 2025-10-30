namespace prjBusTix.Dto.Users;

public class UpdateUserStatusDto
{
    public int Estatus { get; set; }  // 1=Activo, 2=Inactivo, 3=Validado, 4=Pendiente, 5=Cancelado, 6=Suspendido, 7=Bloqueado
}

