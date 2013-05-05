## What is it?
This C# console application takes a CSV export from a digital elevation model (DEM) of a body of water and finds outliers in the data set and writes a new corrected csv file.
An outlier is a data value that doesn't make sense according to it's surrounding data values, and these in elevation terms for a Water Body is a bird or a fish.  The application mentions whether a bird or fish has caused the outlier in elevation.
Additionally, this application calculates the surface area of the Water Body in square km and the volume of water in cubic km.

The outlier's are calculated based on comparing the surrounding 8 elevations, and corrects it by taking the average of them.

## How to use?
Using GIS software, you will need to export the DEM data to a CSV file.  Using a text editor, make sure there is no whitespace in the file at the beginning or end of the file.

Run the Console script and you will see the corrected file if there are any outliers in the same directory as the script.