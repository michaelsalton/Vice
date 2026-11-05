# CCC Features

## Camera

- [x] **Head bob tied to distance walked, not time.** Drive the sine with accumulated horizontal distance. A clock-driven bob keeps bouncing when you're pressed against a wall and doesn't speed up when you sprint. Distance-driven bob also gives you footstep timing: one step per half-cycle.
- [ ] **Landing dip.** Hitting the ground pushes the eye down, and a damped spring pulls it back. A damped spring is a value pulled toward a target by a force proportional to distance, with drag so it settles instead of oscillating forever. Without it, landing from a jump has no weight.
- [ ] **Step smoothing.** The CharacterController's step offset moves the capsule onto a stair in a single frame, so the camera pops up every step. Have the eye height catch up to the body over a few frames.
- [ ] **Strafe roll.** Tilt the camera a degree or two in the direction you strafe (Quake and Half-Life do this). It's cheap and adds a lot of feel.
- [ ] **Sprint FOV kick.** Widen the FOV a few degrees while sprinting and ease it back afterwards. The motion at the screen edges reads as speed.
- [ ] **Viewmodel (the held gun).** You already have Pistol and AK47 models. Two problems to solve:
  - A held gun clips into walls because it's real geometry sitting half a meter in front of the eye. The standard fix is rendering it separately with its own FOV and depth, either as a URP overlay camera or a Render Objects feature.
  - Sway: the gun lags behind mouse movement and bobs with the walk cycle.
- [ ] **4:3 aspect and a lower FOV (60–70).** CRT framing. Pillarbox the 16:9 window, or render 4:3 and scale it up.
- [ ] **Low internal resolution with point upscale.** Render to a small render target and upscale with nearest-neighbour filtering. Saga's `PixelCamera` does this, but its snapping only works with an orthographic camera (verified earlier), so first person would need a perspective version or the snapping turned off.

## Character

- [x] **Crouch.** Your input map already has the action. Shrink the capsule height and center, and before standing back up, do a capsule cast upward. Without that check you can stand up into a ceiling and the controller pushes you out sideways or through the geometry.
- [ ] **Coyote time and jump buffering.**
  - Coyote time: allow a jump for about 100 ms after walking off a ledge.
  - Jump buffering: remember a jump pressed about 100 ms before landing.
  - Both forgive a frame-perfect timing window that players read as "the jump didn't work".
- [ ] **Slope sliding.** The CharacterController refuses to climb past its slope limit but never slides you down steep slopes. Read the ground normal in `OnControllerColliderHit` (the surface direction at the contact point) and add a downhill push when it's too steep.
- [ ] **Interact raycast.** Also already in your input map. Cast a ray from the eye along the camera forward, within arm's length, and show a prompt when it hits something interactable.

## Controls

- [ ] **Stick response curve.** Raise the stick value to a power (squared, cubed) before using it. A linear stick is twitchy for small aim corrections, and the curve gives you fine control near center while keeping full speed at the edge.
- [ ] **Radial deadzone.** Ignore stick input below a magnitude threshold. Handle it as a circle, not per axis: a per-axis deadzone makes aim snap to the horizontal and vertical axes.
- [ ] **Aim acceleration.** Turn speed ramps up the longer the stick is held at full deflection. TimeSplitters and Halo did this. It lets one sensitivity setting cover both precise aiming and fast 180° turns.
- [ ] **Aim assist (slowdown and friction).** Reduce look sensitivity while the crosshair is near a target. PS2-era console shooters depended on this because a 2001 analog stick couldn't aim precisely without help.
- [x] **Walk toggle.** You already have the action; just halve the speed.
