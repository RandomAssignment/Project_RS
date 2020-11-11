public class PlayerController : JoysticController
{
    private void FixedUpdate()
    {
        if (Target != null)
        {
            Target.Move(Stick.localPosition.normalized);
        }
    }
}
