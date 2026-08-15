import os

test_path = r'tests\CommerceAotValidator\Program.cs'
with open(test_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('AIConceptExtractionResult', 'AIIntelligenceExtractionResult')

with open(test_path, 'w', encoding='utf-8') as f:
    f.write(content)
