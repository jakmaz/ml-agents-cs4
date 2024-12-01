# GENERAL

I excluded the checkpoints created in-between starting training and reaching max step (2M in our case) for convenience.

## A NOTE ON THE FOLDER NAMES

I figured it'd be good to have a set way of naming these things. My simple system works as follows:
* [n]P[][] : n Pitches used
* [n][P]B/D/F/V[] : indicates what model is trained
    * B : No **B**ackrays
    * D : **D**ecoupled movement/vision
    * F : **F**air Soccer (AKA **F**ouls punished)
    * S : **S**ound Sensor
    * V : **V**anilla (AKA base model with no changes)
* [n][P][]E : indicates we are using some environmental variable*

Currently, the code only supports (and always did support) only one such variable, which is the ball_touch, configured in the ``yaml`` file.
But oh well. I also didn't bother actually naming them to say how these envs are set, but I'll tell you here that the existing models all have ``ball_touch`` set to 0.02, a somewhat arbitrary decision.

Contact me (Vjosa) if you want to understand more/have any question or remark about this or HATE the naming scheme :)