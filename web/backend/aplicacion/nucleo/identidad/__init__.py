"""Primitivas y casos de uso de identidad de la plataforma web."""

from .seguridad import hash_contrasena, verificar_contrasena

__all__ = ["hash_contrasena", "verificar_contrasena"]
