import base64
import hashlib

from security import make_pin_record, verify_admin_password, verify_pin


def test_pin_hash_is_not_plaintext_and_verifies():
    digest, salt, iterations = make_pin_record("123456")
    assert digest != b"123456" and verify_pin("123456", digest, salt, iterations)
    assert not verify_pin("654321", digest, salt, iterations)


def test_pin_requires_six_digits():
    import pytest

    with pytest.raises(ValueError):
        make_pin_record("abc")


def test_legacy_admin_hash_matches_desktop_format():
    salt = "unit-test-salt"
    password = "correct horse"
    digest = hashlib.sha512(f"{password}:{salt}".encode()).hexdigest()
    assert verify_admin_password(password, "LEGACY_SHA2_512:" + digest, salt)
    assert not verify_admin_password("wrong", "LEGACY_SHA2_512:" + digest, salt)


def test_desktop_pbkdf2_hash_matches():
    password = "correct horse"
    salt = b"0123456789abcdef"
    iterations = 120000
    # The WinForms desktop derives 32 bytes with Rfc2898DeriveBytes.
    digest = hashlib.pbkdf2_hmac("sha1", password.encode(), salt, iterations, dklen=32)
    value = (
        f"PBKDF2${iterations}${base64.b64encode(salt).decode()}${base64.b64encode(digest).decode()}"
    )
    assert verify_admin_password(password, value, None)
