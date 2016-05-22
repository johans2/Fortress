"""Parse the World xslx file and save it as a Unity readable csv."""
from openpyxl import load_workbook

if __name__ == '__main__':
    print "World parser initiated!"
    wb = load_workbook('Chart.xlsx')
    ws = wb["Sheet1"]
    cell_range = 'A1:B2'
    for row in ws.iter_rows(cell_range):
        # print row[0].value
        for cell in row:
            print cell.fill.bgColor.rgb


def parse_color_chart(chart_worksheet):
    """Parse the color chart of the World file."""
    color_dict = {}

    cell_range = 'A1:B2'
    for row in chart_worksheet.iter_rows(cell_range):
        print row[0].value
        color_dict[row[0].value] = row[1].value

    return color_dict


def parse_world():
    """Parse the world."""
    pass


def save_world():
    """Save the World file."""
    pass
