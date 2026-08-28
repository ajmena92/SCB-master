from pydantic import BaseModel, ConfigDict, Field


class ComponenteMenu(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    # El menú histórico admite nombres descriptivos largos; coincide con
    # menu.componente.nombre y evita rechazar datos ya migrados.
    nombre: str = Field(min_length=1, max_length=500)
    tipo: str = Field(default="Principal", max_length=40)
    orden: int = Field(default=1, ge=1, le=20)


class PlantillaMenuEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    semana: int = Field(ge=1, le=5)
    dia: int = Field(ge=1, le=5)
    titulo: str = Field(min_length=1, max_length=160)
    observaciones: str | None = Field(default=None, max_length=500)
    activo: bool = True
    componentes: list[ComponenteMenu] = Field(default_factory=list, max_length=20)


class PlantillaMenuSalida(PlantillaMenuEntrada):
    id_plantilla: int = Field(serialization_alias="idPlantilla")
