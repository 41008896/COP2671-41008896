import difflib
import os

# Function to read the content of a file
def read_file(file):
    with open(file, 'r', encoding='utf-8', errors='ignore') as f:
        return f.readlines()

# Function to get the list of all files in a directory, including subdirectories
def get_files_in_directory(directory):
    files = {}
    for dirpath, dirnames, filenames in os.walk(directory):
        for filename in filenames:
            filepath = os.path.join(dirpath, filename)
            # Store files with relative paths from the root of the directory
            rel_path = os.path.relpath(filepath, directory)
            files[rel_path] = filepath
    return files

# Function to compare directories and print diffs for files in dir1
def compare_directories(dir1, dir2, output_file):
    # Get list of all files in both directories
    files1 = get_files_in_directory(dir1)
    files2 = get_files_in_directory(dir2)

    # Find files only in dir1 (missing in dir2)
    only_in_dir1 = set(files1.keys()) - set(files2.keys())

    # Find common files and compare them
    common_files = set(files1.keys()) & set(files2.keys())

    total_diff_files = 0
    total_diff_lines = 0

    # Start building output
    output = []

    # Files only in dir1 (not in dir2)
    for file in only_in_dir1:
        file_path = files1[file]
        file_lines = read_file(file_path)
        total_diff_files += 1
        total_diff_lines += len(file_lines)
        output.append(f"File {file} is only in dir1. Line count: {len(file_lines)}")

    # Compare common files
    for file in common_files:
        file1_path = files1[file]
        file2_path = files2[file]
        
        # Read both files
        file1_lines = read_file(file1_path)
        file2_lines = read_file(file2_path)

        # Check if they are different
        if file1_lines != file2_lines:
            total_diff_files += 1
            # Count differing lines
            diff = list(difflib.unified_diff(file1_lines, file2_lines))
            differing_lines = len(diff)
            total_diff_lines += differing_lines
            output.append(f"File {file} is different. Line count of differences: {differing_lines}")

    # Write output to the specified file
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(f"Total differing files: {total_diff_files}\n")
        f.write(f"Total differing lines: {total_diff_lines}\n\n")
        f.write("\n".join(output))

# Example usage
dir1 = 'C:\\Users\\GOD\\OneDrive - Santa Fe College\\COP2671-41008896\\Diff\\FinalProject'
dir2 = 'C:\\Users\\GOD\\OneDrive - Santa Fe College\\COP2671-41008896\\Diff\\Origonal'
output_file = 'C:\\Users\\GOD\\OneDrive - Santa Fe College\\COP2671-41008896\\Diff\\Script\\DiffShort.txt'

compare_directories(dir1, dir2, output_file)
