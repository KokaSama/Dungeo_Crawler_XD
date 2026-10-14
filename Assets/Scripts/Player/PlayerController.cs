using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento del jugador con CharacterController.
/// - Entrada: WASD / flechas / stick izquierdo (Input System, acción creada por código).
/// - El "adelante" es el de la cámara (aplanado en el plano XZ), así W siempre sube por la pantalla.
/// - El personaje gira suavemente hacia la dirección en la que se mueve.
/// - Gravedad simple para que se mantenga pegado al suelo.
/// Las colisiones con paredes y obstáculos las resuelve el CharacterController contra los colliders de los prefabs.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 6f;
    [Tooltip("Velocidad de giro hacia la dirección de movimiento (grados por segundo).")]
    [SerializeField] private float velocidadGiro = 720f;

    [Header("Gravedad")]
    [SerializeField] private float gravedad = -25f;

    [Header("Referencias")]
    [Tooltip("Cámara que define el 'adelante' del movimiento. Si se deja vacío se usa la Main Camera.")]
    [SerializeField] private Transform camara;

    private CharacterController controlador;
    private InputAction accionMover;
    private float velocidadVertical;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        accionMover = CrearAccionMover();
    }

    private void OnEnable() => accionMover.Enable();
    private void OnDisable() => accionMover.Disable();
    private void OnDestroy() => accionMover?.Dispose();

    private void Start()
    {
        if (camara == null && Camera.main != null)
            camara = Camera.main.transform;
    }

    private void Update()
    {
        Vector2 entrada = accionMover.ReadValue<Vector2>();
        Vector3 direccion = DireccionRelativaACamara(entrada);

        GirarHacia(direccion);

        // Gravedad: si estamos en el suelo mantenemos una pequeña fuerza hacia abajo para seguir "pegados".
        if (controlador.isGrounded && velocidadVertical < 0f)
            velocidadVertical = -2f;
        velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimiento = direccion * velocidad + Vector3.up * velocidadVertical;
        controlador.Move(movimiento * Time.deltaTime);
    }

    // Convierte el input (x = lateral, y = adelante/atrás) en una dirección en el mundo según hacia dónde mira la cámara.
    private Vector3 DireccionRelativaACamara(Vector2 entrada)
    {
        if (entrada.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 adelante = camara != null ? camara.forward : Vector3.forward;
        adelante.y = 0f;
        if (adelante.sqrMagnitude < 0.0001f)
            adelante = Vector3.forward;
        adelante.Normalize();

        Vector3 derecha = Vector3.Cross(Vector3.up, adelante);
        return Vector3.ClampMagnitude(adelante * entrada.y + derecha * entrada.x, 1f);
    }

    private void GirarHacia(Vector3 direccion)
    {
        if (direccion.sqrMagnitude < 0.0001f)
            return;

        Quaternion objetivo = Quaternion.LookRotation(direccion, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, objetivo, velocidadGiro * Time.deltaTime);
    }

    // Crea la acción "Mover" sin necesidad de un asset .inputactions: teclado (WASD y flechas) + mando.
    private static InputAction CrearAccionMover()
    {
        var accion = new InputAction("Mover", InputActionType.Value);

        accion.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        accion.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        accion.AddBinding("<Gamepad>/leftStick");
        return accion;
    }
}
