"""Parse the World xslx file and save it as a Unity readable csv."""
from openpyxl import load_workbook
from collections import namedtuple

Tile = namedtuple("Tile", "TileType")

def parse_color_chart(chart_worksheet):
    """Parse the color chart of the World file."""
    color_dict = {}
    for row in chart_worksheet.rows:
        color_dict[row[1].fill.bgColor.rgb] = row[0].value

    return color_dict


def parse_world(world_worksheet, color_dict):
    """Parse the world."""
    world = []

    for i in color_dict:
        print i, color_dict[i]

    for row in world_worksheet.rows:
        parsed_row = []
        for cell in row:
            tile = Tile(TileType=color_dict[cell.fill.bgColor.rgb])
            parsed_row.append(tile)
        world.append(parsed_row)
    return world


def save_world(world):
    """Save the World file."""
    
    pass


if __name__ == '__main__':
    print "World parser initiated!"
    wb = load_workbook('World.xlsx')
    color_chart_sheet = wb["ColorChart"]
    world_worksheet = wb["World"]
    color_dict = parse_color_chart(color_chart_sheet)
    world = parse_world(world_worksheet, color_dict)
