# AbstractImageGeneration
*A program designed to procedurally generate simple abstract triangle wallpapers*

This is a simple .NET Winforms program designed to generate abstract triangle images based on Delaunay triangulation and basic particle simulation. There are three image styles:

- **Triangles**: Generate an image with triangles on an ascending brightness scale starting from the top left corner of the image down to the lower right.
- **Net**: Generate an image featuring dots interconnected by lines
- **Dots**: Generate an image featuring large multicolored dots that, over multiple sim iterations, generates a motion blur effect (depending on selected brightness)

# Controls

There are some controls you can use to tweak your final image:

- Resolution
- Margin (which isn't a margin applied to the image itself, but to the simulation behind it. The margin exists because if there is none set, the image's corners would be blank when triangulating particles. With this extra space, the image is zoomed in to hide these)
- Number of particles
- Max Speed (Particle speed is always random, but you can nevertheless limit the maximum a particle can ever go)
- Primary color (Defines a color to use when generating the image. On Triangles mode, the triangles are painted this color. On Net mode, the net nodes are painted this color)
- Brightness

Additionally, there is an "animate" checkbox that iterates through the simulation and plays it out as if it was an animation.

# Exporting

To export your image, you can click the "Copy to clipboard" button and paste it in your image editor of choice to do any manual tweaking you would like

# Copyright

This program makes use of the [Computational Geometry Unity Library](https://github.com/Habrador/Computational-geometry) source code to perform triangulation. 
This software is licensed under the MIT license. You may copy it, share it, adapt it, and more however you like under the terms of the license. 

See the LICENSE.txt file for details.
