//
// $Id$
//
//
// Original author: Jennifer Leclaire <leclaire@uni-mainz.de>
//
// Copyright 2019 Institute of Computer Science, Johannes Gutenberg-Universit�t Mainz
//
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
//
#ifndef _Datastructures_triMS5_HPP_
#define _Datastructures_triMS5_HPP_

//#include "../mz5/Datastructures_mz5.hpp"
#include "Configuration_triMS5.hpp"
#include "../MSDataFile.hpp"
#include <string>

namespace pwiz {
namespace msdata {
namespace triMS5 {

	//using namespace pwiz::msdata::mz5;

/**
* General triMS5 file information.
* This struct contains information about the mz5 version, and how to handle specific data sets.
*/
struct FileInformation_triMS5_Data
{
	FileInformation_triMS5_Data() : majorVersion(Configuration_triMS5::triMS5_FILE_MAJOR_VERSION), minorVersion(Configuration_triMS5::triMS5_FILE_MINOR_VERSION) {}
	unsigned short majorVersion;
	unsigned short minorVersion;
};

struct FileInformation_triMS5 : public FileInformation_triMS5_Data
{
	FileInformation_triMS5() : FileInformation_triMS5_Data() {}
	~FileInformation_triMS5() = default;
	FileInformation_triMS5(const FileInformation_triMS5& fi);
	FileInformation_triMS5(const Configuration_triMS5& c);
	FileInformation_triMS5& operator=(const FileInformation_triMS5& fi);
	static H5::CompType getType();
};


struct SpectrumListIndices_triMS5_Data
{
	SpectrumListIndices_triMS5_Data() = default;
	SpectrumListIndices_triMS5_Data(unsigned int p, unsigned int l): presetScanConfigurationIndex(p), localSpectrumIndex(l){}
	unsigned int presetScanConfigurationIndex;
	unsigned int localSpectrumIndex;
};

struct SpectrumListIndices_triMS5 : public SpectrumListIndices_triMS5_Data
{
	SpectrumListIndices_triMS5() = default;
	SpectrumListIndices_triMS5(unsigned int p, unsigned int l) : SpectrumListIndices_triMS5_Data(p, l) {}
	~SpectrumListIndices_triMS5() = default;
	
	bool operator==(const SpectrumListIndices_triMS5& other) const
	{
		return presetScanConfigurationIndex == other.presetScanConfigurationIndex && localSpectrumIndex == other.localSpectrumIndex;
	}
	static H5::CompType getType();
};
}
}
}

#endif /*_Configuration_triMS5_HPP_*/